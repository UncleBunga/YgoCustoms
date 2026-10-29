--Slasher - The Messenger
local s,id=GetID()
function s.initial_effect(c)
    c:EnableCounterPermit(0x199)
    -- Flip effect: Destroy 2 cards or opponent pays 1000 LP
    local e1=Effect.CreateEffect(c)
    e1:SetDescription(aux.Stringid(id,0))
    e1:SetCategory(CATEGORY_DESTROY)
    e1:SetType(EFFECT_TYPE_SINGLE+EFFECT_TYPE_FLIP)
    e1:SetProperty(EFFECT_FLAG_CARD_TARGET)
    e1:SetTarget(s.destg)
    e1:SetOperation(s.desop)
    c:RegisterEffect(e1)

    -- Cannot be destroyed by battle
    local e2=Effect.CreateEffect(c)
    e2:SetType(EFFECT_TYPE_SINGLE)
    e2:SetCode(EFFECT_INDESTRUCTABLE_BATTLE)
    e2:SetValue(1)
    c:RegisterEffect(e2)

    -- Once per turn: Change this card to face-down Defense Position
    local e3=Effect.CreateEffect(c)
    e3:SetDescription(aux.Stringid(id,3))
    e3:SetCategory(CATEGORY_POSITION)
    e3:SetType(EFFECT_TYPE_IGNITION)
    e3:SetRange(LOCATION_MZONE)
    e3:SetCountLimit(1)
    e3:SetTarget(s.fdtg)
    e3:SetOperation(s.fdop)
    c:RegisterEffect(e3)
end

function s.destg(e,tp,eg,ep,ev,re,r,rp,chk,chkc)
    if chkc then return chkc:IsOnField() and chkc:IsControler(1-tp) end
    if chk==0 then return Duel.IsExistingTarget(aux.TRUE,tp,0,LOCATION_ONFIELD,2,nil) end

    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_DESTROY)
    local g=Duel.SelectTarget(tp,aux.TRUE,tp,0,LOCATION_ONFIELD,2,2,nil)
    Duel.SetOperationInfo(0,CATEGORY_DESTROY,g,2,0,0)
end

function s.desop(e,tp,eg,ep,ev,re,r,rp)
    local g=Duel.GetChainInfo(0,CHAININFO_TARGET_CARDS)
    local tg=g:Filter(Card.IsRelateToEffect,nil,e)

    -- Check if opponent can pay 1000 LP
    local can_pay = Duel.CheckLPCost(1-tp,1000)

    -- Opponent may pay 1000 LP to negate this effect
    if can_pay and Duel.SelectYesNo(1-tp,aux.Stringid(id,1)) then
        Duel.PayLPCost(1-tp,1000)
    else
        if #tg>0 then
            Duel.Destroy(tg,REASON_EFFECT)
        end
    end
end

-- Self-flip target/operation
function s.fdtg(e,tp,eg,ep,ev,re,r,rp,chk)
    local c=e:GetHandler()
    if chk==0 then return c:IsFaceup() and c:IsCanChangePosition() end
end

function s.fdop(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    if c:IsRelateToEffect(e) and c:IsFaceup() then
        Duel.ChangePosition(c,POS_FACEDOWN_DEFENSE,POS_FACEDOWN_DEFENSE,POS_FACEDOWN_DEFENSE,POS_FACEDOWN_DEFENSE)
    end
end