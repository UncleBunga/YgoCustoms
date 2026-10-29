-- Slasher's Hunting Grounds
local s,id=GetID()
function s.initial_effect(c)
    c:EnableCounterPermit(0x199,LOCATION_FZONE)
    -- Activate Field Spell
    local e1=Effect.CreateEffect(c)
    e1:SetType(EFFECT_TYPE_ACTIVATE)
    e1:SetCode(EVENT_FREE_CHAIN)
    c:RegisterEffect(e1)

    -- Place 1 Kill Counter when a Slasher monster destroys any card by battle or card effect
    local e2=Effect.CreateEffect(c)
    e2:SetDescription(aux.Stringid(id,0))
    e2:SetCategory(CATEGORY_COUNTER)
    e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_TRIGGER_F)
    e2:SetCode(EVENT_DESTROYED)
    e2:SetRange(LOCATION_FZONE)
    e2:SetCondition(s.ctcon)
    e2:SetOperation(s.ctop)
    c:RegisterEffect(e2)

    -- Slasher monsters gain 100 ATK for each Kill Counter on the field
    local e3=Effect.CreateEffect(c)
    e3:SetType(EFFECT_TYPE_FIELD)
    e3:SetCode(EFFECT_UPDATE_ATTACK)
    e3:SetRange(LOCATION_FZONE)
    e3:SetTargetRange(LOCATION_MZONE,0)
    e3:SetTarget(aux.TargetBoolFunction(Card.IsSetCard,0x991))
    e3:SetValue(s.atkval)
    c:RegisterEffect(e3)

    -- Quick Effect: Remove 3 Kill Counters from your field to negate the activation (Once per Chain)
    local e4=Effect.CreateEffect(c)
    e4:SetDescription(aux.Stringid(id,1))
    e4:SetCategory(CATEGORY_NEGATE)
    e4:SetType(EFFECT_TYPE_QUICK_O)
    e4:SetCode(EVENT_CHAINING)
    e4:SetProperty(EFFECT_FLAG_DAMAGE_STEP+EFFECT_FLAG_DAMAGE_CAL)
    e4:SetRange(LOCATION_FZONE)
    e4:SetCountLimit(1,id,EFFECT_COUNT_CODE_CHAIN)
    e4:SetCondition(s.negcon)
    e4:SetCost(s.negcost)
    e4:SetTarget(s.negtg)
    e4:SetOperation(s.negop)
    c:RegisterEffect(e4)
end
s.counter_list={0x199}

-- Condition: Check if a "Slasher" monster destroyed a card by battle or effect
function s.ctcon(e,tp,eg,ep,ev,re,r,rp)
    if not e:GetHandler():IsCanAddCounter(0x199,1) then return false end
    for tc in aux.Next(eg) do
        if tc:IsPreviousControler(1-tp) then
            if tc:IsReason(REASON_EFFECT) then
                local re2=tc:GetReasonEffect()
                local p=re2 and re2:GetHandler()
                if p and p:IsSetCard(0x991) and p:IsMonster() and p:IsControler(tp) then return true end
            end
            if tc:IsReason(REASON_BATTLE) then
                local rc=tc:GetReasonCard()
                if rc and rc:IsSetCard(0x991) and rc:IsControler(tp) then return true end
            end
        end
    end
    return false
end

-- Operation: Add 1 Kill Counter
function s.ctop(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    if c:IsRelateToEffect(e) then
        c:AddCounter(0x199,1)
    end
end

-- ATK Value Calculation (Counts all Kill Counters on the field)
function s.atkval(e,c)
    local g=Duel.GetMatchingGroup(function(tc) return tc:GetCounter(0x199)>0 end,e:GetHandlerPlayer(),LOCATION_ONFIELD,LOCATION_ONFIELD,nil)
    local ct=0
    for tc in aux.Next(g) do
        ct=ct+tc:GetCounter(0x199)
    end
    return ct*100
end

-- Negation Condition
function s.negcon(e,tp,eg,ep,ev,re,r,rp)
    return rp~=tp and Duel.IsChainNegatable(ev)
end

-- Negation Cost: Remove 3 Kill Counters from your field
function s.negcost(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.IsCanRemoveCounter(tp,1,0,0x199,3,REASON_COST) end
    Duel.RemoveCounter(tp,1,0,0x199,3,REASON_COST)
end

-- Negation Target
function s.negtg(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return true end
    Duel.SetOperationInfo(0,CATEGORY_NEGATE,eg,1,0,0)
end

-- Negation Operation (Negates only)
function s.negop(e,tp,eg,ep,ev,re,r,rp)
    Duel.NegateActivation(ev)
end