-- Slasher - Preordained Design
local s,id=GetID()
function s.initial_effect(c)
    c:EnableCounterPermit(0x199)
    -- 3 "Slasher" monsters; must first be Special Summoned by sending them from your field to the GY
    c:EnableReviveLimit()
    Fusion.AddProcMixN(c,true,true,aux.FilterBoolFunction(Card.IsSetCard,0x991),3)
    Fusion.AddContactProc(c,s.contactfil,s.contactop,s.splimit)

    -- Cannot be targeted for attacks, or by opponent's card effects
    local e1=Effect.CreateEffect(c)
    e1:SetType(EFFECT_TYPE_SINGLE)
    e1:SetProperty(EFFECT_FLAG_SINGLE_RANGE)
    e1:SetRange(LOCATION_MZONE)
    e1:SetCode(EFFECT_CANNOT_BE_EFFECT_TARGET)
    e1:SetValue(aux.tgoval)
    c:RegisterEffect(e1)
    local e2=Effect.CreateEffect(c)
    e2:SetType(EFFECT_TYPE_SINGLE)
    e2:SetProperty(EFFECT_FLAG_SINGLE_RANGE)
    e2:SetRange(LOCATION_MZONE)
    e2:SetCode(EFFECT_CANNOT_BE_BATTLE_TARGET)
    e2:SetValue(aux.imval1)
    c:RegisterEffect(e2)

    -- If opponent activates a Spell/Trap Card or effect: destroy all their monsters in that card's column
    local e3=Effect.CreateEffect(c)
    e3:SetDescription(aux.Stringid(id,0))
    e3:SetCategory(CATEGORY_DESTROY)
    e3:SetType(EFFECT_TYPE_QUICK_F)
    e3:SetCode(EVENT_CHAINING)
    e3:SetProperty(EFFECT_FLAG_DAMAGE_STEP+EFFECT_FLAG_DAMAGE_CAL)
    e3:SetRange(LOCATION_MZONE)
    e3:SetCondition(s.colcon)
    e3:SetTarget(s.coltg)
    e3:SetOperation(s.colop)
    c:RegisterEffect(e3)

    -- Once per turn: remove 2 Kill Counters from anywhere on the field; destroy 1 card on the field
    local e4=Effect.CreateEffect(c)
    e4:SetDescription(aux.Stringid(id,1))
    e4:SetCategory(CATEGORY_DESTROY)
    e4:SetType(EFFECT_TYPE_IGNITION)
    e4:SetRange(LOCATION_MZONE)
    e4:SetCountLimit(1)
    e4:SetCost(s.descost)
    e4:SetTarget(s.destg)
    e4:SetOperation(s.desop)
    c:RegisterEffect(e4)
end
s.counter_list={0x199}

function s.contactfil(tp)
    return Duel.GetMatchingGroup(Card.IsAbleToGraveAsCost,tp,LOCATION_ONFIELD,0,nil)
end

function s.contactop(g)
    Duel.SendtoGrave(g,REASON_COST+REASON_MATERIAL)
end

function s.splimit(e,se,sp,st)
    return not e:GetHandler():IsLocation(LOCATION_EXTRA)
end

function s.colcon(e,tp,eg,ep,ev,re,r,rp)
    return rp==1-tp and re:IsSpellTrapEffect() and re:GetHandler():IsLocation(LOCATION_ONFIELD)
end

function s.colgroup(tp,re)
    local rc=re:GetHandler()
    if not rc:IsLocation(LOCATION_ONFIELD) then return Group.CreateGroup() end
    return rc:GetColumnGroup():Filter(Card.IsControler,nil,1-tp):Filter(Card.IsLocation,nil,LOCATION_MZONE)
end

function s.coltg(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return true end
    local g=s.colgroup(tp,re)
    Duel.SetOperationInfo(0,CATEGORY_DESTROY,g,#g,0,0)
end

function s.colop(e,tp,eg,ep,ev,re,r,rp)
    local g=s.colgroup(tp,re)
    if #g>0 then
        Duel.Destroy(g,REASON_EFFECT)
    end
end

function s.descost(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.IsCanRemoveCounter(tp,1,1,0x199,2,REASON_COST) end
    Duel.RemoveCounter(tp,1,1,0x199,2,REASON_COST)
end

function s.destg(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.IsExistingMatchingCard(nil,tp,LOCATION_ONFIELD,LOCATION_ONFIELD,1,nil) end
    Duel.SetOperationInfo(0,CATEGORY_DESTROY,nil,1,PLAYER_ALL,LOCATION_ONFIELD)
end

function s.desop(e,tp,eg,ep,ev,re,r,rp)
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_DESTROY)
    local g=Duel.SelectMatchingCard(tp,nil,tp,LOCATION_ONFIELD,LOCATION_ONFIELD,1,1,nil)
    if #g>0 then
        Duel.HintSelection(g)
        Duel.Destroy(g,REASON_EFFECT)
    end
end
